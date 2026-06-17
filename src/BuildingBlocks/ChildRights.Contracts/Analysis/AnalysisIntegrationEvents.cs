using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Contracts.Analysis;

/// <summary>
/// Raised by the Analysis service whenever a red flag is produced (by rules or AI).
/// Consumed by the Notifications service, which dispatches it to the target audiences.
/// </summary>
public sealed record RedFlagRaisedIntegrationEvent : IntegrationEvent
{
    public required Guid FlagId { get; init; }

    public required string RuleCode { get; init; }

    public required AnalysisScope Scope { get; init; }

    public required Guid SubjectId { get; init; }

    public required string SubjectName { get; init; }

    public required FlagSeverity Severity { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required Agency SourceAgency { get; init; }

    public required IReadOnlyCollection<AudienceRole> TargetAudiences { get; init; } = [];

    public required IReadOnlyCollection<string> RecommendedActions { get; init; } = [];
}

/// <summary>
/// Raised by the Analysis service when a profiling recommendation is issued
/// (e.g. suggested specialisation for grade 10, or courses for a community academy).
/// </summary>
public sealed record RecommendationIssuedIntegrationEvent : IntegrationEvent
{
    public required Guid RecommendationId { get; init; }

    public required AnalysisScope Scope { get; init; }

    public required Guid SubjectId { get; init; }

    public required string Kind { get; init; }

    public required string Summary { get; init; }
}
