using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Contracts.JuvenilePolice;

/// <summary>
/// Raised by the Juvenile Police service when a bullying report is filed for a class.
/// A class-level signal: bullying is a risk to the whole class, not just one pupil.
/// </summary>
public sealed record BullyingReportFiledIntegrationEvent : IntegrationEvent
{
    public required Guid ReportId { get; init; }

    public required Guid ClassId { get; init; }

    public required Guid SchoolId { get; init; }

    public required FlagSeverity Severity { get; init; }

    public required string Summary { get; init; }
}
