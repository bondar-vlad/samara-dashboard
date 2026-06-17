using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Flags.Commands;

/// <summary>Marks a red flag as acknowledged by a responsible adult/officer.</summary>
public sealed record AcknowledgeRedFlagCommand(Guid FlagId) : ICommand;

/// <summary>Marks a red flag as resolved.</summary>
public sealed record ResolveRedFlagCommand(Guid FlagId) : ICommand;

internal sealed class AcknowledgeRedFlagCommandHandler(IAnalysisDbContext context)
    : ICommandHandler<AcknowledgeRedFlagCommand>
{
    public async Task<Result> Handle(AcknowledgeRedFlagCommand command, CancellationToken cancellationToken)
    {
        var flag = await context.RedFlags.FirstOrDefaultAsync(f => f.Id == command.FlagId, cancellationToken);
        if (flag is null)
        {
            return Result.Failure(Error.NotFound($"Red flag '{command.FlagId}' was not found."));
        }

        flag.Acknowledge();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ResolveRedFlagCommandHandler(IAnalysisDbContext context)
    : ICommandHandler<ResolveRedFlagCommand>
{
    public async Task<Result> Handle(ResolveRedFlagCommand command, CancellationToken cancellationToken)
    {
        var flag = await context.RedFlags.FirstOrDefaultAsync(f => f.Id == command.FlagId, cancellationToken);
        if (flag is null)
        {
            return Result.Failure(Error.NotFound($"Red flag '{command.FlagId}' was not found."));
        }

        flag.Resolve();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
