using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.JuvenilePolice.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.JuvenilePolice.Api.Persistence;

public sealed class JuvenilePoliceDbContext(DbContextOptions<JuvenilePoliceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<BullyingReport> BullyingReports => Set<BullyingReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("juvenile");

        var report = modelBuilder.Entity<BullyingReport>();
        report.ToTable("bullying_reports");
        report.HasKey(r => r.Id);
        report.Ignore(r => r.DomainEvents);
        report.Property(r => r.Severity).HasConversion<string>().HasMaxLength(20);
        report.Property(r => r.Summary).HasMaxLength(1000).IsRequired();
        report.HasIndex(r => r.ClassId);

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Seeds a couple of demo bullying reports for the lyceum's named 9-А / 10-А classes.</summary>
internal sealed class JuvenilePoliceDataSeeder(JuvenilePoliceDbContext context) : IDataSeeder
{
    private static readonly Guid LyceumId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Class9 = Guid.Parse("22222222-2222-2222-2222-222222220009");
    private static readonly Guid Class10 = Guid.Parse("22222222-2222-2222-2222-222222220010");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.BullyingReports.AnyAsync(cancellationToken))
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        context.BullyingReports.AddRange(
            new BullyingReport(Guid.NewGuid(), Class9, LyceumId, FlagSeverity.Yellow,
                "Конфлікт між учнями під час перерви; класному керівнику доручено спостереження.", today.AddDays(-10)),
            new BullyingReport(Guid.NewGuid(), Class10, LyceumId, FlagSeverity.Orange,
                "Повторні випадки цькування в класному онлайн-чаті; залучено психолога.", today.AddDays(-4)));

        await context.SaveChangesAsync(cancellationToken);
    }
}
