using System.Reflection;
using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Infrastructure.Persistence;

/// <summary>EF Core unit of work for the Education bounded context.</summary>
public sealed class EducationDbContext(DbContextOptions<EducationDbContext> options)
    : DbContext(options), IEducationDbContext, IUnitOfWork
{
    public DbSet<Student> Students => Set<Student>();

    public DbSet<School> Schools => Set<School>();

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("education");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
