using ChildRights.BuildingBlocks.Domain.Primitives;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Enums;

namespace ChildRights.Analysis.Domain.Entities;

/// <summary>
/// An audit record of a single analysis execution — capturing how it was triggered,
/// which model was used and what it produced. Supports the three execution modes:
/// event, scheduled and on-demand.
/// </summary>
public sealed class AnalysisRun : AggregateRoot
{
    private AnalysisRun()
    {
    }

    public AnalysisRun(
        Guid id,
        AnalysisTrigger trigger,
        AnalysisScope scope,
        Guid subjectId,
        string modelName,
        DateTime startedAtUtc)
        : base(id)
    {
        Trigger = trigger;
        Scope = scope;
        SubjectId = subjectId;
        ModelName = modelName;
        StartedAtUtc = startedAtUtc;
        Status = RunStatus.Pending;
    }

    public AnalysisTrigger Trigger { get; private set; }

    public AnalysisScope Scope { get; private set; }

    public Guid SubjectId { get; private set; }

    public string ModelName { get; private set; } = string.Empty;

    public RunStatus Status { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public int FlagsProduced { get; private set; }

    public int RecommendationsProduced { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public void Complete(int flagsProduced, int recommendationsProduced, string summary, DateTime completedAtUtc)
    {
        Status = RunStatus.Completed;
        FlagsProduced = flagsProduced;
        RecommendationsProduced = recommendationsProduced;
        Summary = summary;
        CompletedAtUtc = completedAtUtc;
    }

    public void Fail(string reason, DateTime completedAtUtc)
    {
        Status = RunStatus.Failed;
        Summary = reason;
        CompletedAtUtc = completedAtUtc;
    }
}
