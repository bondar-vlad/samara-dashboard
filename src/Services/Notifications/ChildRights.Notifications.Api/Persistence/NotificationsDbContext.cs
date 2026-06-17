using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Notifications.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Notifications.Api.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Referral> Referrals => Set<Referral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");

        var notification = modelBuilder.Entity<Notification>();
        notification.ToTable("notifications");
        notification.HasKey(n => n.Id);
        notification.Ignore(n => n.DomainEvents);
        notification.Property(n => n.SubjectName).HasMaxLength(200);
        notification.Property(n => n.Audience).HasMaxLength(40);
        notification.Property(n => n.Severity).HasMaxLength(20);
        notification.Property(n => n.Title).HasMaxLength(300);
        notification.Property(n => n.Message).HasMaxLength(2000);
        notification.Property(n => n.Status).HasMaxLength(20);
        notification.HasIndex(n => n.SubjectId);

        var referral = modelBuilder.Entity<Referral>();
        referral.ToTable("referrals");
        referral.HasKey(r => r.Id);
        referral.Ignore(r => r.DomainEvents);
        referral.Property(r => r.FromAgency).HasConversion<string>().HasMaxLength(30);
        referral.Property(r => r.ToAgency).HasConversion<string>().HasMaxLength(30);
        referral.Property(r => r.SubjectName).HasMaxLength(200);
        referral.Property(r => r.Severity).HasMaxLength(20);
        referral.Property(r => r.Reason).HasMaxLength(2000);
        referral.Property(r => r.Status).HasMaxLength(20);
        referral.HasIndex(r => r.ToAgency);

        base.OnModelCreating(modelBuilder);
    }
}
