using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Analysis;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Entities;

namespace ChildRights.Analysis.Application.Flags.Commands;

/// <summary>
/// Persists a red flag and broadcasts it. Used by the agency event consumers
/// (medical concerns, bullying reports) to raise cross-agency signals.
/// </summary>
public sealed record RaiseRedFlagCommand(
    string RuleCode,
    AnalysisScope Scope,
    Guid SubjectId,
    string SubjectName,
    FlagSeverity Severity,
    string Title,
    string Description,
    Agency SourceAgency,
    IReadOnlyCollection<AudienceRole> TargetAudiences,
    IReadOnlyCollection<string> RecommendedActions,
    string ModelName = "rule-based-v1") : ICommand<Guid>;

internal sealed class RaiseRedFlagCommandHandler(
    IAnalysisDbContext context,
    IEventPublisher eventPublisher,
    IClock clock) : ICommandHandler<RaiseRedFlagCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RaiseRedFlagCommand command, CancellationToken cancellationToken)
    {
        var flag = new RedFlag(
            Guid.NewGuid(),
            command.RuleCode,
            command.Scope,
            command.SubjectId,
            command.SubjectName,
            command.Severity,
            command.Title,
            command.Description,
            command.SourceAgency,
            command.TargetAudiences,
            command.RecommendedActions,
            command.ModelName,
            clock.UtcNow);

        context.RedFlags.Add(flag);
        await context.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new RedFlagRaisedIntegrationEvent
            {
                FlagId = flag.Id,
                RuleCode = flag.RuleCode,
                Scope = flag.Scope,
                SubjectId = flag.SubjectId,
                SubjectName = flag.SubjectName,
                Severity = flag.Severity,
                Title = flag.Title,
                Description = flag.Description,
                SourceAgency = flag.SourceAgency,
                TargetAudiences = flag.TargetAudiences,
                RecommendedActions = flag.RecommendedActions
            },
            cancellationToken);

        return flag.Id;
    }
}
