using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Runs.Queries;

/// <summary>
/// Whether a pupil has been analysed yet, and when last. Lets the UI distinguish
/// "analysed, no risks found" from "not analysed yet" and offer a run/re-run action.
/// </summary>
public sealed record GetStudentAnalysisStatusQuery(Guid StudentId) : IQuery<StudentAnalysisStatusDto>;

public sealed record StudentAnalysisStatusDto(bool Analyzed, DateTime? LastAnalyzedUtc, int RunCount);

internal sealed class GetStudentAnalysisStatusQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<GetStudentAnalysisStatusQuery, StudentAnalysisStatusDto>
{
    public async Task<Result<StudentAnalysisStatusDto>> Handle(
        GetStudentAnalysisStatusQuery query,
        CancellationToken cancellationToken)
    {
        var completed = context.AnalysisRuns
            .Where(r => r.Scope == AnalysisScope.Student
                && r.SubjectId == query.StudentId
                && r.Status == RunStatus.Completed);

        var runCount = await completed.CountAsync(cancellationToken);
        var last = await completed
            .OrderByDescending(r => r.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var lastUtc = last is null ? (DateTime?)null : last.CompletedAtUtc ?? last.StartedAtUtc;
        return Result.Success(new StudentAnalysisStatusDto(last is not null, lastUtc, runCount));
    }
}
