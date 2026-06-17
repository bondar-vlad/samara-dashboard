using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Application.Abstractions;

/// <summary>
/// Orchestrates a single pupil analysis: pulls data from Education, runs the selected
/// model, persists the run, flags and recommendations, and publishes integration events.
/// Reused by the on-demand API, the event consumers and the scheduler.
/// </summary>
public interface IAnalysisEngine
{
    Task<AnalysisRunResultDto> AnalyzeStudentAsync(
        Guid studentId,
        AnalysisTrigger trigger,
        string? modelName = null,
        AnalysisKind kind = AnalysisKind.Profile,
        CancellationToken cancellationToken = default);
}
