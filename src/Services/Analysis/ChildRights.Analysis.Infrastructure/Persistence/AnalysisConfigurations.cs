using System.Text.Json;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChildRights.Analysis.Infrastructure.Persistence;

internal sealed class RedFlagConfiguration : IEntityTypeConfiguration<RedFlag>
{
    public void Configure(EntityTypeBuilder<RedFlag> builder)
    {
        builder.ToTable("red_flags");
        builder.HasKey(f => f.Id);
        builder.Ignore(f => f.DomainEvents);

        builder.Property(f => f.RuleCode).HasMaxLength(60).IsRequired();
        builder.Property(f => f.SubjectName).HasMaxLength(200);
        builder.Property(f => f.Title).HasMaxLength(300);
        builder.Property(f => f.Description).HasMaxLength(2000);
        builder.Property(f => f.AiModel).HasMaxLength(80);

        builder.Property(f => f.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Scope).HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.SourceAgency).HasConversion<string>().HasMaxLength(30);
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);

        var audienceConverter = new ValueConverter<List<AudienceRole>, string>(
            value => JsonSerializer.Serialize(value.Select(a => a.ToString()), (JsonSerializerOptions?)null),
            value => (JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
                .Select(Enum.Parse<AudienceRole>).ToList());

        var audienceComparer = new ValueComparer<List<AudienceRole>>(
            (left, right) => left!.SequenceEqual(right!),
            value => value.Aggregate(0, (hash, role) => HashCode.Combine(hash, role.GetHashCode())),
            value => value.ToList());

        builder.Property(f => f.TargetAudiences)
            .HasConversion(audienceConverter, audienceComparer)
            .HasColumnType("text");

        var stringListConverter = new ValueConverter<List<string>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());

        var stringListComparer = new ValueComparer<List<string>>(
            (left, right) => left!.SequenceEqual(right!),
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());

        builder.Property(f => f.RecommendedActions)
            .HasConversion(stringListConverter, stringListComparer)
            .HasColumnType("text");

        builder.HasIndex(f => f.SubjectId);
        builder.HasIndex(f => f.Severity);
        builder.HasIndex(f => f.Status);
    }
}

internal sealed class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("recommendations");
        builder.HasKey(r => r.Id);
        builder.Ignore(r => r.DomainEvents);

        builder.Property(r => r.SubjectName).HasMaxLength(200);
        builder.Property(r => r.Title).HasMaxLength(300);
        builder.Property(r => r.Summary).HasMaxLength(2000);
        builder.Property(r => r.Rationale).HasMaxLength(2000);
        builder.Property(r => r.AiModel).HasMaxLength(80);

        builder.Property(r => r.Scope).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Kind).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(r => r.SubjectId);
        builder.HasIndex(r => r.Scope);
    }
}

internal sealed class AnalysisRunConfiguration : IEntityTypeConfiguration<AnalysisRun>
{
    public void Configure(EntityTypeBuilder<AnalysisRun> builder)
    {
        builder.ToTable("analysis_runs");
        builder.HasKey(r => r.Id);
        builder.Ignore(r => r.DomainEvents);

        builder.Property(r => r.ModelName).HasMaxLength(80);
        builder.Property(r => r.Summary).HasMaxLength(2000);

        builder.Property(r => r.Trigger).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Scope).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(r => r.StartedAtUtc);
    }
}
