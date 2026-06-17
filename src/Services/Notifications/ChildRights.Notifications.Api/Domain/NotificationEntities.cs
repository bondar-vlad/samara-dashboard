using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Notifications.Api.Domain;

/// <summary>A message dispatched to a single audience because of a red flag.</summary>
public sealed class Notification : Entity
{
    private Notification()
    {
    }

    public Notification(
        Guid id,
        Guid flagId,
        Guid subjectId,
        string subjectName,
        string audience,
        string severity,
        string title,
        string message,
        DateTime createdAtUtc)
        : base(id)
    {
        FlagId = flagId;
        SubjectId = subjectId;
        SubjectName = subjectName;
        Audience = audience;
        Severity = severity;
        Title = title;
        Message = message;
        CreatedAtUtc = createdAtUtc;
        Status = "Sent";
    }

    public Guid FlagId { get; private set; }

    public Guid SubjectId { get; private set; }

    public string SubjectName { get; private set; } = string.Empty;

    public string Audience { get; private set; } = string.Empty;

    public string Severity { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string Status { get; private set; } = "Sent";

    public DateTime CreatedAtUtc { get; private set; }
}

/// <summary>A formal hand-off of a case from one agency to another.</summary>
public sealed class Referral : Entity
{
    private Referral()
    {
    }

    public Referral(
        Guid id,
        Agency fromAgency,
        Agency toAgency,
        Guid subjectId,
        string subjectName,
        string severity,
        string reason,
        DateTime createdAtUtc)
        : base(id)
    {
        FromAgency = fromAgency;
        ToAgency = toAgency;
        SubjectId = subjectId;
        SubjectName = subjectName;
        Severity = severity;
        Reason = reason;
        CreatedAtUtc = createdAtUtc;
        Status = "Pending";
    }

    public Agency FromAgency { get; private set; }

    public Agency ToAgency { get; private set; }

    public Guid SubjectId { get; private set; }

    public string SubjectName { get; private set; } = string.Empty;

    public string Severity { get; private set; } = string.Empty;

    public string Reason { get; private set; } = string.Empty;

    public string Status { get; private set; } = "Pending";

    public DateTime CreatedAtUtc { get; private set; }
}
