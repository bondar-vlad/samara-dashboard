using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// A persisted red-flag signal produced by the rule engine or an AI model.
/// Carries the multi-level severity, the audiences who must be informed and the
/// recommended actions.
/// </summary>
public sealed class RedFlag : AggregateRoot
{
    private RedFlag()
    {
    }

    public RedFlag(
        Guid id,
        string ruleCode,
        AnalysisScope scope,
        Guid subjectId,
        string subjectName,
        FlagSeverity severity,
        string title,
        string description,
        Agency sourceAgency,
        IEnumerable<AudienceRole> targetAudiences,
        IEnumerable<string> recommendedActions,
        string aiModel,
        DateTime detectedAtUtc)
        : base(id)
    {
        RuleCode = ruleCode;
        Scope = scope;
        SubjectId = subjectId;
        SubjectName = subjectName;
        Severity = severity;
        Title = title;
        Description = description;
        SourceAgency = sourceAgency;
        TargetAudiences = targetAudiences.ToList();
        RecommendedActions = recommendedActions.ToList();
        AiModel = aiModel;
        DetectedAtUtc = detectedAtUtc;
        Status = FlagStatus.Open;
    }

    public string RuleCode { get; private set; } = string.Empty;

    public AnalysisScope Scope { get; private set; }

    public Guid SubjectId { get; private set; }

    public string SubjectName { get; private set; } = string.Empty;

    public FlagSeverity Severity { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Agency SourceAgency { get; private set; }

    public List<AudienceRole> TargetAudiences { get; private set; } = [];

    public List<string> RecommendedActions { get; private set; } = [];

    /// <summary>Identifier of the model/engine that produced the flag (e.g. "rule-based-v1").</summary>
    public string AiModel { get; private set; } = string.Empty;

    public DateTime DetectedAtUtc { get; private set; }

    public FlagStatus Status { get; private set; }

    public void Acknowledge() => Status = FlagStatus.Acknowledged;

    public void Resolve() => Status = FlagStatus.Resolved;
}
