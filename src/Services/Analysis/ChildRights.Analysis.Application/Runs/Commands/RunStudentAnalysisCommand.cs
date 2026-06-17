using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Application.Runs.Commands;

/// <summary>On-demand (or event/scheduled) analysis of a single pupil with the chosen model.</summary>
public sealed record RunStudentAnalysisCommand(
    Guid StudentId,
    AnalysisTrigger Trigger = AnalysisTrigger.OnDemand,
    string? ModelName = null,
    AnalysisKind? Kind = null) : ICommand<AnalysisRunResultDto>;

internal sealed class RunStudentAnalysisCommandHandler(IAnalysisEngine engine)
    : ICommandHandler<RunStudentAnalysisCommand, AnalysisRunResultDto>
{
    public async Task<Result<AnalysisRunResultDto>> Handle(
        RunStudentAnalysisCommand command,
        CancellationToken cancellationToken)
        => await engine.AnalyzeStudentAsync(
            command.StudentId, command.Trigger, command.ModelName, command.Kind, cancellationToken);
}
