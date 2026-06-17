namespace ChildRights.BuildingBlocks.Domain.SharedKernel;

/// <summary>
/// The level at which an analysis is performed and at which an insight applies.
/// Mirrors the administrative hierarchy of the education system.
/// </summary>
public enum AnalysisScope
{
    Student = 1,
    Class = 2,
    School = 3,
    Community = 4,
    Region = 5,
    Country = 6
}
