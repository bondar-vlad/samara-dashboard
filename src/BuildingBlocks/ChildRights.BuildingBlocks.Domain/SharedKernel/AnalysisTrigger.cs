namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// What initiated an analysis run. The platform supports three execution modes
/// so AI workloads can be both reactive and proactive.
/// </summary>
public enum AnalysisTrigger
{
    /// <summary>Reactive: fired by an incoming data event (e.g. a new attendance record).</summary>
    Event = 1,

    /// <summary>Proactive: fired by the scheduler (e.g. a nightly absence sweep).</summary>
    Scheduled = 2,

    /// <summary>Ad-hoc: requested explicitly by a user through the API.</summary>
    OnDemand = 3
}
