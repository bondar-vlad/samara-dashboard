namespace ChildRights.Contracts.Medical;

/// <summary>
/// Raised by the Medical service when a pupil records frequent occurrences of a
/// condition category that warrants follow-up (e.g. recurring respiratory illness).
/// </summary>
public sealed record MedicalConcernReportedIntegrationEvent : IntegrationEvent
{
    public required Guid StudentId { get; init; }

    public required string StudentName { get; init; }

    public required string ConditionCategory { get; init; }

    public required int OccurrencesInPeriod { get; init; }

    public required string RecommendedReferral { get; init; }
}
