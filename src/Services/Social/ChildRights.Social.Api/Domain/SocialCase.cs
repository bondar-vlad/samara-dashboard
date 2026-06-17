using ChildRights.BuildingBlocks.Domain.Primitives;

namespace ChildRights.Social.Api.Domain;

/// <summary>A social-services case opened for a child, often via an inter-agency referral.</summary>
public sealed class SocialCase : Entity
{
    private SocialCase()
    {
    }

    public SocialCase(
        Guid id,
        Guid subjectId,
        string subjectName,
        string sourceAgency,
        string severity,
        string reason,
        DateOnly openedOn)
        : base(id)
    {
        SubjectId = subjectId;
        SubjectName = subjectName;
        SourceAgency = sourceAgency;
        Severity = severity;
        Reason = reason;
        OpenedOn = openedOn;
        Status = "Open";
    }

    public Guid SubjectId { get; private set; }

    public string SubjectName { get; private set; } = string.Empty;

    public string SourceAgency { get; private set; } = string.Empty;

    public string Severity { get; private set; } = string.Empty;

    public string Reason { get; private set; } = string.Empty;

    public string Status { get; private set; } = "Open";

    public DateOnly OpenedOn { get; private set; }
}
