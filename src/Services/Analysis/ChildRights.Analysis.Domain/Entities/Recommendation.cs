using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>A profiling recommendation for a pupil, school, community or region.</summary>
public sealed class Recommendation : AggregateRoot
{
    private Recommendation()
    {
    }

    public Recommendation(
        Guid id,
        AnalysisScope scope,
        Guid subjectId,
        string subjectName,
        RecommendationKind kind,
        string title,
        string summary,
        string rationale,
        double confidence,
        string aiModel,
        DateTime createdAtUtc)
        : base(id)
    {
        Scope = scope;
        SubjectId = subjectId;
        SubjectName = subjectName;
        Kind = kind;
        Title = title;
        Summary = summary;
        Rationale = rationale;
        Confidence = confidence;
        AiModel = aiModel;
        CreatedAtUtc = createdAtUtc;
    }

    public AnalysisScope Scope { get; private set; }

    public Guid SubjectId { get; private set; }

    public string SubjectName { get; private set; } = string.Empty;

    public RecommendationKind Kind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Rationale { get; private set; } = string.Empty;

    /// <summary>Confidence 0..1 reported by the model/engine.</summary>
    public double Confidence { get; private set; }

    public string AiModel { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }
}
