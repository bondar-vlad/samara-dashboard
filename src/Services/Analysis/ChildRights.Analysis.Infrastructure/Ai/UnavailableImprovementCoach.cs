using ChildRights.Analysis.Application.Abstractions;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// The default improvement coach used when no LLM is configured. It is never invoked (callers
/// check <see cref="IsAvailable"/> first) and exists so the dependency always resolves and the
/// UI can show a clear "AI model not connected" message.
/// </summary>
internal sealed class UnavailableImprovementCoach : IImprovementCoach
{
    public bool IsAvailable => false;

    public string ModelName => "(none)";

    public Task<ImprovementCoachResult> CoachAsync(
        ImprovementCoachRequest request,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("No AI model is connected for the improvement coach.");
}
