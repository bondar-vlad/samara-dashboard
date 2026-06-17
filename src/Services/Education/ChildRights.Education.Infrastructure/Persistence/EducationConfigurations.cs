using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace ChildRights.Education.Infrastructure.Persistence;

internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");
        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.Direction);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Community).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Region).HasMaxLength(200).IsRequired();
        builder.Property(s => s.InstitutionType).HasConversion<string>().HasMaxLength(40).IsRequired();

        builder.HasMany(s => s.OfferedProfiles)
            .WithOne()
            .HasForeignKey(o => o.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.OfferedProfiles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class SchoolProfileOfferingConfiguration : IEntityTypeConfiguration<SchoolProfileOffering>
{
    public void Configure(EntityTypeBuilder<SchoolProfileOffering> builder)
    {
        builder.ToTable("school_profile_offerings");
        builder.HasKey(o => o.Id);
        builder.Ignore(o => o.DomainEvents);
        builder.Property(o => o.Profile).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.HasIndex(o => new { o.SchoolId, o.Profile }).IsUnique();
        builder.HasIndex(o => o.Profile);
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
        builder.Ignore(s => s.HasProfileMismatch);
        builder.Ignore(s => s.DesiredCluster);
        builder.Ignore(s => s.RecommendedCluster);
        builder.Property(s => s.FullName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.DeclaredProfile).HasConversion<string>().HasMaxLength(40);

        var profileListConverter = new ValueConverter<IReadOnlyList<EducationProfile>, string>(
            value => JsonSerializer.Serialize(value.Select(p => p.ToString()), (JsonSerializerOptions?)null),
            value => (JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
                .Select(Enum.Parse<EducationProfile>).ToList());

        var profileListComparer = new ValueComparer<IReadOnlyList<EducationProfile>>(
            (left, right) => left!.SequenceEqual(right!),
            value => value.Aggregate(0, (hash, profile) => HashCode.Combine(hash, profile.GetHashCode())),
            value => value.ToList());

        builder.Property(s => s.DesiredProfiles)
            .HasConversion(profileListConverter, profileListComparer)
            .HasColumnType("text");

        builder.Property(s => s.RecommendedProfiles)
            .HasConversion(profileListConverter, profileListComparer)
            .HasColumnType("text");

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
        builder.Property(g => g.Topic).HasMaxLength(160);
        builder.Property(g => g.Term).HasMaxLength(40).IsRequired();
        builder.HasIndex(g => g.StudentId);
        builder.HasIndex(g => new { g.StudentId, g.Subject });
    }
}
