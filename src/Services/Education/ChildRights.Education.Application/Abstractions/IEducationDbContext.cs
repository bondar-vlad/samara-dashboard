using ChildRights.Education.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Abstractions;

/// <summary>
/// Application-facing abstraction over the Education persistence store. Keeps the
/// application layer free of a concrete EF Core <c>DbContext</c> implementation.
/// </summary>
public interface IEducationDbContext
{
    DbSet<Student> Students { get; }

    DbSet<School> Schools { get; }

    DbSet<SchoolClass> Classes { get; }

    DbSet<AttendanceRecord> AttendanceRecords { get; }

    DbSet<Grade> Grades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
