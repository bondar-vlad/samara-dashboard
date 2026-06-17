using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using ChildRights.Analysis.Domain.Enums;
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

/// <summary>
/// The pupil's admission inputs, supplied only for admission goals (4th НМТ subject and
/// direction). Kept off the profile/risk goals so each analysis sees only what it needs.
/// </summary>
public sealed record AdmissionInputs(
    IReadOnlyDictionary<NmtSubject, int> NmtScores,
    NmtSubject? ChosenFourthSubject,
    string? DesiredDirectionCode,
    IReadOnlyList<AdmissionDirection> Directions);

/// <summary>
/// Input to an analysis model: a single pupil snapshot plus the <see cref="AnalysisGoal"/>
/// that focuses the analysis on one concrete case. <see cref="Admission"/> is populated only
/// for admission goals.
/// </summary>
public sealed record AnalysisRequest(
    StudentSnapshot Snapshot,
    AnalysisGoal Goal = AnalysisGoal.StudentRisk,
    AdmissionInputs? Admission = null);

/// <summary>Output of an analysis model: the findings plus the model identity.</summary>
public sealed record AnalysisResult(
    string ModelName,
    IReadOnlyList<FlagFinding> Flags,
    IReadOnlyList<RecommendationFinding> Recommendations,
    string Summary);
