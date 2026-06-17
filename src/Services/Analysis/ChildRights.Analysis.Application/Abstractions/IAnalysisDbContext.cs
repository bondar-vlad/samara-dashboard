using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Abstractions;

/// <summary>Application-facing abstraction over the Analysis persistence store.</summary>
public interface IAnalysisDbContext
{
    DbSet<RedFlag> RedFlags { get; }

    DbSet<Recommendation> Recommendations { get; }

    DbSet<AnalysisRun> AnalysisRuns { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
