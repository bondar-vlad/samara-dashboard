using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Flags.Queries;

/// <summary>Lists red flags filtered by severity, scope, subject and/or status.</summary>
public sealed record ListRedFlagsQuery(
    string? Severity = null,
    string? Scope = null,
    Guid? SubjectId = null,
    string? Status = null) : IQuery<IReadOnlyCollection<RedFlagDto>>;

internal sealed class ListRedFlagsQueryHandler(IAnalysisDbContext context)
    : IQueryHandler<ListRedFlagsQuery, IReadOnlyCollection<RedFlagDto>>
{
    public async Task<Result<IReadOnlyCollection<RedFlagDto>>> Handle(
        ListRedFlagsQuery query,
        CancellationToken cancellationToken)
    {
        var flagsQuery = context.RedFlags.AsQueryable();

        if (query.SubjectId is { } subjectId)
        {
            flagsQuery = flagsQuery.Where(f => f.SubjectId == subjectId);
        }

        var flags = await flagsQuery
            .OrderByDescending(f => f.DetectedAtUtc)
            .ToListAsync(cancellationToken);

        IEnumerable<Domain.Entities.RedFlag> filtered = flags;

        if (!string.IsNullOrWhiteSpace(query.Severity))
        {
            filtered = filtered.Where(f => string.Equals(f.Severity.ToString(), query.Severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Scope))
        {
            filtered = filtered.Where(f => string.Equals(f.Scope.ToString(), query.Scope, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            filtered = filtered.Where(f => string.Equals(f.Status.ToString(), query.Status, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyCollection<RedFlagDto> result = filtered.Select(f => f.ToDto()).ToList();
        return Result.Success(result);
    }
}
