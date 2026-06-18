namespace ChildRights.Analysis.Application.Improvement;

/// <summary>
/// A pupil-facing improvement plan: what to pull up, in which subjects and topics, to make the
/// pupil's <b>chosen</b> profile/direction realistic. Produced by the AI coach; when no model is
/// connected, <see cref="Available"/> is <c>false</c> and <see cref="Message"/> explains it.
/// </summary>
public sealed record StudentImprovementPlanDto(
    Guid StudentId,
    bool Available,
    string ModelName,
    string TargetKind,
    string TargetName,
    bool HasChoice,
    bool IsMismatch,
    string Summary,
    IReadOnlyList<ImprovementItemDto> Items,
    IReadOnlyList<string> Steps,
    string? Message);

/// <summary>One area to pull up: the measured gap plus the AI's concrete advice for it.</summary>
public sealed record ImprovementItemDto(
    string Area,
    string Name,
    double Current,
    double Target,
    double Gap,
    string Advice);
