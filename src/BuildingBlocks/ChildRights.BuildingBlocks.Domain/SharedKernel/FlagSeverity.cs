namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// Multi-level "traffic light" severity of a red-flag signal.
/// Designed to be tiered (not just yellow/red) so escalation policies can differ per level.
/// </summary>
public enum FlagSeverity
{
    /// <summary>No concern detected.</summary>
    Green = 0,

    /// <summary>Advisory: worth watching, usually handled inside the school.</summary>
    Yellow = 1,

    /// <summary>Elevated concern: typically notifies parents / class teacher / administration.</summary>
    Orange = 2,

    /// <summary>Urgent: requires cross-agency action (social services, juvenile police, medical).</summary>
    Red = 3
}
