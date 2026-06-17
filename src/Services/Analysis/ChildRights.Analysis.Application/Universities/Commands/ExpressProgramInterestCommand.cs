using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Universities.Commands;

/// <summary>
/// Records a pupil's interest in a specific specialty. Stored so the platform can send
/// <b>depersonalised</b> demand to universities. Idempotent per (student, programme).
/// </summary>
public sealed record ExpressProgramInterestCommand(Guid StudentId, Guid ProgramId) : ICommand<Guid>;

internal sealed class ExpressProgramInterestCommandHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient,
    IClock clock) : ICommandHandler<ExpressProgramInterestCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ExpressProgramInterestCommand command, CancellationToken cancellationToken)
    {
        var program = await context.UniversityPrograms
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (program is null)
        {
            return Result.Failure<Guid>(Error.NotFound($"Programme '{command.ProgramId}' was not found."));
        }

        var existing = await context.ProgramInterests
            .FirstOrDefaultAsync(i => i.StudentId == command.StudentId && i.ProgramId == command.ProgramId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var profile = await educationClient.GetStudentProfileAsync(command.StudentId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound($"Student '{command.StudentId}' was not found in the education service."));
        }

        var interest = new ProgramInterest(
            Guid.NewGuid(),
            command.StudentId,
            profile.SchoolId,
            program.UniversityId,
            program.UniversityName,
            program.Id,
            program.Name,
            clock.UtcNow);

        context.ProgramInterests.Add(interest);
        await context.SaveChangesAsync(cancellationToken);

        return interest.Id;
    }
}
