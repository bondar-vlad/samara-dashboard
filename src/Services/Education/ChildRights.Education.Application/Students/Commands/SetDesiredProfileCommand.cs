using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Education.Application.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Commands;

/// <summary>
/// Records the profiles a pupil self-reports they want. Per the reform, all chosen
/// profiles must belong to a single cluster (several profiles within one cluster).
/// </summary>
public sealed record SetDesiredProfilesCommand(Guid StudentId, IReadOnlyList<EducationProfile> DesiredProfiles)
    : ICommand;

public sealed class SetDesiredProfilesCommandValidator : AbstractValidator<SetDesiredProfilesCommand>
{
    public SetDesiredProfilesCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.DesiredProfiles)
            .NotEmpty().WithMessage("At least one desired profile is required.");
        RuleForEach(x => x.DesiredProfiles).IsInEnum();
        RuleFor(x => x.DesiredProfiles)
            .Must(profiles => ProfileTaxonomy.AreInSameCluster(profiles))
            .When(x => x.DesiredProfiles is { Count: > 0 })
            .WithMessage("All desired profiles must belong to the same cluster.");
    }
}

internal sealed class SetDesiredProfilesCommandHandler(IEducationDbContext context)
    : ICommandHandler<SetDesiredProfilesCommand>
{
    public async Task<Result> Handle(SetDesiredProfilesCommand command, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == command.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure(Error.NotFound($"Student '{command.StudentId}' was not found."));
        }

        student.SetDesiredProfiles(command.DesiredProfiles);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
