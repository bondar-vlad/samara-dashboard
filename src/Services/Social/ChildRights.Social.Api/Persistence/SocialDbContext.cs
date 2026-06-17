using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Social.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Social.Api.Persistence;

public sealed class SocialDbContext(DbContextOptions<SocialDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<SocialCase> Cases => Set<SocialCase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("social");

        var socialCase = modelBuilder.Entity<SocialCase>();
        socialCase.ToTable("cases");
        socialCase.HasKey(c => c.Id);
        socialCase.Ignore(c => c.DomainEvents);
        socialCase.Property(c => c.SubjectName).HasMaxLength(200);
        socialCase.Property(c => c.SourceAgency).HasMaxLength(40);
        socialCase.Property(c => c.Severity).HasMaxLength(20);
        socialCase.Property(c => c.Reason).HasMaxLength(1000);
        socialCase.Property(c => c.Status).HasMaxLength(20);
        socialCase.HasIndex(c => c.SubjectId);

        base.OnModelCreating(modelBuilder);
    }
}
