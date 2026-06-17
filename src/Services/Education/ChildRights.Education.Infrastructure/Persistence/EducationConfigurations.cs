using ChildRights.Education.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChildRights.Education.Infrastructure.Persistence;

internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");
        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.DomainEvents);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Community).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Region).HasMaxLength(200).IsRequired();
    }
}

internal sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("classes");
        builder.HasKey(c => c.Id);
        builder.Ignore(c => c.DomainEvents);
        builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
        builder.Property(c => c.ClassTeacher).HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.SchoolId);
    }
}

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.DomainEvents);
        builder.Property(s => s.FullName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.DeclaredProfile).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(s => s.SchoolId);
        builder.HasIndex(s => s.ClassId);
    }
}

internal sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_records");
        builder.HasKey(a => a.Id);
        builder.Ignore(a => a.DomainEvents);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Subject).HasMaxLength(120);
        builder.HasIndex(a => new { a.StudentId, a.Date });
    }
}

internal sealed class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("grades");
        builder.HasKey(g => g.Id);
        builder.Ignore(g => g.DomainEvents);
        builder.Property(g => g.Subject).HasMaxLength(120).IsRequired();
        builder.Property(g => g.Term).HasMaxLength(40).IsRequired();
        builder.HasIndex(g => g.StudentId);
    }
}
