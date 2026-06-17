using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Contracts.Notifications;

/// <summary>
/// Raised when a case must be formally referred from one agency to another
/// (e.g. excessive absences escalate from Education to Social Services / Juvenile Police).
/// </summary>
public sealed record InterAgencyReferralRequestedIntegrationEvent : IntegrationEvent
{
    public required Guid ReferralId { get; init; }

    public required Agency FromAgency { get; init; }

    public required Agency ToAgency { get; init; }

    public required Guid SubjectId { get; init; }

    public required string SubjectName { get; init; }

    public required FlagSeverity Severity { get; init; }

    public required string Reason { get; init; }
}
