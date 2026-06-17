using System.Reflection;
using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Infrastructure.Persistence;

/// <summary>EF Core unit of work for the Analysis bounded context.</summary>
public sealed class AnalysisDbContext(DbContextOptions<AnalysisDbContext> options)
    : DbContext(options), IAnalysisDbContext, IUnitOfWork
{
    public DbSet<RedFlag> RedFlags => Set<RedFlag>();

    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    public DbSet<AnalysisRun> AnalysisRuns => Set<AnalysisRun>();

    public DbSet<University> Universities => Set<University>();

    public DbSet<UniversityProgram> UniversityPrograms => Set<UniversityProgram>();

    public DbSet<StudentProfileInsight> StudentProfileInsights => Set<StudentProfileInsight>();

    public DbSet<ProgramInterest> ProgramInterests => Set<ProgramInterest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analysis");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
