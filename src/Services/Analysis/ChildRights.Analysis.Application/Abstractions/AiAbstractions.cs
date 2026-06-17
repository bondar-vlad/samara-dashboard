using ChildRights.Analysis.Domain.Rules;

namespace ChildRights.Analysis.Application.Abstractions;

/// <summary>
/// The strategy abstraction over an analysis model. Implementations include a
/// deterministic rule-based engine (always available) and AI/LLM-backed providers.
/// Swapping models is a configuration concern, never a code change in callers.
/// </summary>
public interface IAiAnalysisProvider
{
    /// <summary>Stable identifier of the model/engine, persisted with every result for auditing.</summary>
    string ModelName { get; }

    Task<AnalysisResult> AnalyzeAsync(AnalysisRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Selects an <see cref="IAiAnalysisProvider"/> by name, falling back to the configured default.</summary>
public interface IAiAnalysisProviderFactory
{
    IReadOnlyCollection<string> AvailableModels { get; }

    IAiAnalysisProvider Resolve(string? modelName = null);
}

/// <summary>Input to an analysis model — a single pupil snapshot.</summary>
public sealed record AnalysisRequest(StudentSnapshot Snapshot);

/// <summary>Output of an analysis model: the findings plus the model identity.</summary>
public sealed record AnalysisResult(
    string ModelName,
    IReadOnlyList<FlagFinding> Flags,
    IReadOnlyList<RecommendationFinding> Recommendations,
    string Summary);
