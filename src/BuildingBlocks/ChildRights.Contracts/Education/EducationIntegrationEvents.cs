using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Contracts.Education;

/// <summary>
/// Raised by the Education service when a pupil's unexcused absences cross a
/// configured threshold (e.g. 10 lessons). Consumed by the Analysis service.
/// </summary>
public sealed record AttendanceThresholdReachedIntegrationEvent : IntegrationEvent
{
    public required Guid StudentId { get; init; }

    public required string StudentName { get; init; }

    public required Guid ClassId { get; init; }

    public required Guid SchoolId { get; init; }

    public required int UnexcusedAbsences { get; init; }

    public required string Period { get; init; }
}

/// <summary>
/// Raised by the Education service when a pupil's grade trend declines across one
/// or more subjects. Consumed by the Analysis service for profiling and red flags.
/// </summary>
public sealed record GradesDeclinedIntegrationEvent : IntegrationEvent
{
    public required Guid StudentId { get; init; }

    public required string StudentName { get; init; }

    public required Guid SchoolId { get; init; }

    public required IReadOnlyCollection<string> AffectedSubjects { get; init; } = [];

    public required decimal AverageDrop { get; init; }
}
