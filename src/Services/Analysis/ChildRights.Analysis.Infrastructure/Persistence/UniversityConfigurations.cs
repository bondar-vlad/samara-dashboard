using System.Text.Json;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChildRights.Analysis.Infrastructure.Persistence;

internal static class JsonColumns
{
    public static ValueConverter<List<string>, string> StringList { get; } = new(
        value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
        value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());

    public static ValueComparer<List<string>> StringListComparer { get; } = new(
        (left, right) => left!.SequenceEqual(right!),
        value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        value => value.ToList());

    public static ValueConverter<List<EducationProfile>, string> ProfileList { get; } = new(
        value => JsonSerializer.Serialize(value.Select(p => p.ToString()), (JsonSerializerOptions?)null),
        value => (JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
            .Select(Enum.Parse<EducationProfile>).ToList());

    public static ValueComparer<List<EducationProfile>> ProfileListComparer { get; } = new(
        (left, right) => left!.SequenceEqual(right!),
        value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        value => value.ToList());

    public static ValueConverter<List<TopicStrength>, string> TopicStrengthList { get; } = new(
        value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
        value => JsonSerializer.Deserialize<List<TopicStrength>>(value, (JsonSerializerOptions?)null) ?? new List<TopicStrength>());

    public static ValueComparer<List<TopicStrength>> TopicStrengthListComparer { get; } = new(
        (left, right) => left!.SequenceEqual(right!),
        value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
        value => value.ToList());
}

internal sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.ToTable("universities");
        builder.HasKey(u => u.Id);
        builder.Ignore(u => u.DomainEvents);
        builder.Property(u => u.Name).HasMaxLength(300).IsRequired();
        builder.Property(u => u.City).HasMaxLength(120);
        builder.Property(u => u.Region).HasMaxLength(200);
    }
}

internal sealed class UniversityProgramConfiguration : IEntityTypeConfiguration<UniversityProgram>
{
    public void Configure(EntityTypeBuilder<UniversityProgram> builder)
    {
        builder.ToTable("university_programs");
        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.DomainEvents);
        builder.Property(p => p.UniversityName).HasMaxLength(300);
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Cluster).HasConversion<string>().HasMaxLength(40);

        builder.Property(p => p.RelevantProfiles)
            .HasConversion(JsonColumns.ProfileList, JsonColumns.ProfileListComparer).HasColumnType("text");
        builder.Property(p => p.KeySubjects)
            .HasConversion(JsonColumns.StringList, JsonColumns.StringListComparer).HasColumnType("text");
        builder.Property(p => p.KeyTopics)
            .HasConversion(JsonColumns.StringList, JsonColumns.StringListComparer).HasColumnType("text");

        builder.HasIndex(p => p.UniversityId);
        builder.HasIndex(p => p.Cluster);
    }
}

internal sealed class StudentProfileInsightConfiguration : IEntityTypeConfiguration<StudentProfileInsight>
{
    public void Configure(EntityTypeBuilder<StudentProfileInsight> builder)
    {
        builder.ToTable("student_profile_insights");
        builder.HasKey(i => i.Id);
        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.IsMismatch);
        builder.Property(i => i.RecommendedCluster).HasConversion<string>().HasMaxLength(40);
        builder.Property(i => i.DesiredCluster).HasConversion<string>().HasMaxLength(40);

        builder.Property(i => i.RecommendedProfiles)
            .HasConversion(JsonColumns.ProfileList, JsonColumns.ProfileListComparer).HasColumnType("text");
        builder.Property(i => i.TopicStrengths)
            .HasConversion(JsonColumns.TopicStrengthList, JsonColumns.TopicStrengthListComparer).HasColumnType("text");

        builder.HasIndex(i => i.StudentId).IsUnique();
        builder.HasIndex(i => i.RecommendedCluster);
    }
}

internal sealed class ProgramInterestConfiguration : IEntityTypeConfiguration<ProgramInterest>
{
    public void Configure(EntityTypeBuilder<ProgramInterest> builder)
    {
        builder.ToTable("program_interests");
        builder.HasKey(i => i.Id);
        builder.Ignore(i => i.DomainEvents);
        builder.Property(i => i.UniversityName).HasMaxLength(300);
        builder.Property(i => i.ProgramName).HasMaxLength(300);
        builder.HasIndex(i => new { i.StudentId, i.ProgramId }).IsUnique();
        builder.HasIndex(i => i.ProgramId);
    }
}
