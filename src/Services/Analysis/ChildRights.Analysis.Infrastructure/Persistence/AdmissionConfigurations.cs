using System.Text.Json;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChildRights.Analysis.Infrastructure.Persistence;

internal static class AdmissionJsonColumns
{
    public static ValueConverter<Dictionary<NmtSubject, double>, string> CoefficientMap { get; } = new(
        value => JsonSerializer.Serialize(
            value.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value), (JsonSerializerOptions?)null),
        value => (JsonSerializer.Deserialize<Dictionary<string, double>>(value, (JsonSerializerOptions?)null)
                  ?? new Dictionary<string, double>())
            .ToDictionary(kv => Enum.Parse<NmtSubject>(kv.Key), kv => kv.Value));

    public static ValueComparer<Dictionary<NmtSubject, double>> CoefficientMapComparer { get; } = new(
        (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
        value => value.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
        value => value.ToDictionary(kv => kv.Key, kv => kv.Value));

    public static ValueConverter<Dictionary<NmtSubject, int>, string> ScoreMap { get; } = new(
        value => JsonSerializer.Serialize(
            value.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value), (JsonSerializerOptions?)null),
        value => (JsonSerializer.Deserialize<Dictionary<string, int>>(value, (JsonSerializerOptions?)null)
                  ?? new Dictionary<string, int>())
            .ToDictionary(kv => Enum.Parse<NmtSubject>(kv.Key), kv => kv.Value));

    public static ValueComparer<Dictionary<NmtSubject, int>> ScoreMapComparer { get; } = new(
        (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
        value => value.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
        value => value.ToDictionary(kv => kv.Key, kv => kv.Value));
}

internal sealed class AdmissionDirectionConfiguration : IEntityTypeConfiguration<AdmissionDirection>
{
    public void Configure(EntityTypeBuilder<AdmissionDirection> builder)
    {
        builder.ToTable("admission_directions");
        builder.HasKey(d => d.Id);
        builder.Ignore(d => d.DomainEvents);

        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.BranchOfKnowledge).HasMaxLength(200);
        builder.Property(d => d.RelatedCluster).HasConversion<string>().HasMaxLength(40);

        builder.Property(d => d.NmtCoefficients)
            .HasConversion(AdmissionJsonColumns.CoefficientMap, AdmissionJsonColumns.CoefficientMapComparer)
            .HasColumnType("text");
        builder.Property(d => d.KeySubjects)
            .HasConversion(JsonColumns.StringList, JsonColumns.StringListComparer).HasColumnType("text");
        builder.Property(d => d.KeyTopics)
            .HasConversion(JsonColumns.StringList, JsonColumns.StringListComparer).HasColumnType("text");

        builder.HasMany(d => d.Specialties)
            .WithOne()
            .HasForeignKey(s => s.DirectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Specialties).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(d => d.Code).IsUnique();
    }
}

internal sealed class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("specialties");
        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.DomainEvents);
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.DirectionId);
    }
}

internal sealed class StudentAdmissionChoiceConfiguration : IEntityTypeConfiguration<StudentAdmissionChoice>
{
    public void Configure(EntityTypeBuilder<StudentAdmissionChoice> builder)
    {
        builder.ToTable("student_admission_choices");
        builder.HasKey(c => c.Id);
        builder.Ignore(c => c.DomainEvents);

        builder.Property(c => c.ChosenFourthSubject).HasConversion<string>().HasMaxLength(40);
        builder.Property(c => c.DesiredDirectionCode).HasMaxLength(20);

        builder.Property(c => c.NmtScores)
            .HasConversion(AdmissionJsonColumns.ScoreMap, AdmissionJsonColumns.ScoreMapComparer)
            .HasColumnType("text");

        builder.HasIndex(c => c.StudentId).IsUnique();
    }
}
