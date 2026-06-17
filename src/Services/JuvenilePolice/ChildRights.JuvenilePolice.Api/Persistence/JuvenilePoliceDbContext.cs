using ChildRights.BuildingBlocks.Application.Abstractions;
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
