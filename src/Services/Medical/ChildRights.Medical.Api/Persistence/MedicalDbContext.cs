using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Medical.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Medical.Api.Persistence;

public sealed class MedicalDbContext(DbContextOptions<MedicalDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<MedicalVisit> Visits => Set<MedicalVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("medical");

        var visit = modelBuilder.Entity<MedicalVisit>();
        visit.ToTable("visits");
        visit.HasKey(v => v.Id);
        visit.Ignore(v => v.DomainEvents);
        visit.Property(v => v.StudentName).HasMaxLength(200).IsRequired();
        visit.Property(v => v.ConditionCategory).HasMaxLength(120).IsRequired();
        visit.Property(v => v.Note).HasMaxLength(500);
        visit.HasIndex(v => v.StudentId);

        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Seeds two respiratory visits for pupil "Oleh" so a third POST triggers a medical concern.</summary>
internal sealed class MedicalDataSeeder(MedicalDbContext context) : IDataSeeder
{
    private static readonly Guid OlehId = Guid.Parse("33333333-3333-3333-3333-333333331001");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Visits.AnyAsync(cancellationToken))
        {
            return;
        }

        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);
        context.Visits.AddRange(
            new MedicalVisit(Guid.NewGuid(), OlehId, "Петренко Олег", "Респіраторні захворювання", start, "ГРВІ"),
            new MedicalVisit(Guid.NewGuid(), OlehId, "Петренко Олег", "Респіраторні захворювання", start.AddDays(14), "Бронхіт"));

        await context.SaveChangesAsync(cancellationToken);
    }
}
