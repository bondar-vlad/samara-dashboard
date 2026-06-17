using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Commands;

/// <summary>
/// Writes the AI-recommended profiles back onto the pupil record. Invoked by the
/// integration-event consumer when the Analysis service issues a profile recommendation,
/// establishing the link between a pupil and their recommended profiles (one cluster).
/// </summary>
public sealed record ApplyProfileRecommendationCommand(
    Guid StudentId,
    IReadOnlyList<EducationProfile> RecommendedProfiles,
    double Confidence) : ICommand;

internal sealed class ApplyProfileRecommendationCommandHandler(IEducationDbContext context, IClock clock)
    : ICommandHandler<ApplyProfileRecommendationCommand>
{
    public async Task<Result> Handle(ApplyProfileRecommendationCommand command, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == command.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure(Error.NotFound($"Student '{command.StudentId}' was not found."));
        }

        student.ApplyRecommendation(command.RecommendedProfiles, command.Confidence, clock.UtcNow);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
