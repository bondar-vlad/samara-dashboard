using ChildRights.Analysis.Domain.Improvement;

namespace ChildRights.Analysis.Application.Abstractions;

/// <summary>
/// What the AI coach is asked to advise on: the pupil's chosen target plus the measured gaps
/// toward it. No personal identifiers are included (data minimisation).
/// </summary>
public sealed record ImprovementCoachRequest(
    int GradeLevel,
    string TargetKind,
    string TargetName,
    IReadOnlyList<ImprovementGap> Gaps,
    IReadOnlyList<string> Strengths);

/// <summary>Concrete study advice for a single area to pull up.</summary>
public sealed record ImprovementAdvice(string Name, string Advice);

/// <summary>The coach's plan: an overall summary, per-area advice and general study steps.</summary>
public sealed record ImprovementCoachResult(
    string Summary,
    IReadOnlyList<ImprovementAdvice> Items,
    IReadOnlyList<string> Steps);

/// <summary>
/// An AI coach that turns a measured gap analysis toward a pupil's <b>chosen</b> profile or
/// admission direction into concrete, encouraging study advice ("what to pull up, in which
/// subjects and topics"). It is backed by an LLM only: when no model is connected
/// <see cref="IsAvailable"/> is <c>false</c> and callers surface a "model not connected"
/// message rather than inventing canned advice.
/// </summary>
public interface IImprovementCoach
{
    /// <summary>True only when a real AI model is connected and ready to produce a plan.</summary>
    bool IsAvailable { get; }

    /// <summary>Identifier of the backing model (for auditing/UX).</summary>
    string ModelName { get; }

    Task<ImprovementCoachResult> CoachAsync(
        ImprovementCoachRequest request,
        CancellationToken cancellationToken = default);
}
