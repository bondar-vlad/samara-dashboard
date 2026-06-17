using ChildRights.BuildingBlocks.Domain.SharedKernel;

namespace ChildRights.Education.Domain.Policies;

/// <summary>
/// Encodes the attendance red-flag rules as a pure, testable domain policy.
/// Tiered thresholds drive the multi-level (yellow → orange → red) escalation:
/// at 10 unexcused absences the parents/administration are notified; at 20 the
/// case is escalated to social services / juvenile police.
/// </summary>
public static class AttendancePolicy
{
    public const int YellowThreshold = 5;
    public const int OrangeThreshold = 10;
    public const int RedThreshold = 20;

    public static FlagSeverity Evaluate(int unexcusedAbsences) => unexcusedAbsences switch
    {
        >= RedThreshold => FlagSeverity.Red,
        >= OrangeThreshold => FlagSeverity.Orange,
        >= YellowThreshold => FlagSeverity.Yellow,
        _ => FlagSeverity.Green
    };

    /// <summary>True once the pupil reaches the level where parents/administration must be notified.</summary>
    public static bool RequiresNotification(int unexcusedAbsences) =>
        Evaluate(unexcusedAbsences) >= FlagSeverity.Orange;
}
