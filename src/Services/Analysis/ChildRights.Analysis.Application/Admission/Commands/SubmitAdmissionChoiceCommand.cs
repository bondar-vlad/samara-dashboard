using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Application.Admission.Commands;

/// <summary>
/// Submits or updates a pupil's admission inputs: НМТ scores, chosen 4th subject and desired
/// direction. All parts are optional so the two widgets can be filled independently.
/// </summary>
public sealed record SubmitAdmissionChoiceCommand(
    Guid StudentId,
    IReadOnlyDictionary<NmtSubject, int>? NmtScores = null,
    NmtSubject? ChosenFourthSubject = null,
    string? DesiredDirectionCode = null) : ICommand;

public sealed class SubmitAdmissionChoiceCommandValidator : AbstractValidator<SubmitAdmissionChoiceCommand>
{
    public SubmitAdmissionChoiceCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.ChosenFourthSubject!.Value)
            .Must(NmtSubjectCatalog.IsFourthSubjectOption)
            .When(x => x.ChosenFourthSubject is not null)
            .WithMessage("The chosen subject is not a valid 4th НМТ subject option.");
        RuleForEach(x => x.NmtScores)
            .Must(kv => kv.Value is >= 100 and <= 200)
            .When(x => x.NmtScores is not null)
            .WithMessage("НМТ scores must be between 100 and 200.");
    }
}

internal sealed class SubmitAdmissionChoiceCommandHandler(
    IAnalysisDbContext context,
    IEducationDataClient educationClient,
    IClock clock) : ICommandHandler<SubmitAdmissionChoiceCommand>
{
    public async Task<Result> Handle(SubmitAdmissionChoiceCommand command, CancellationToken cancellationToken)
    {
        if (command.DesiredDirectionCode is { } code &&
            !await context.AdmissionDirections.AnyAsync(d => d.Code == code, cancellationToken))
        {
            return Result.Failure(Error.Validation($"Unknown admission direction '{code}'."));
        }

        var choice = await context.StudentAdmissionChoices
            .FirstOrDefaultAsync(c => c.StudentId == command.StudentId, cancellationToken);

        if (choice is null)
        {
            var profile = await educationClient.GetStudentProfileAsync(command.StudentId, cancellationToken);
            if (profile is null)
            {
                return Result.Failure(Error.NotFound(
                    $"Student '{command.StudentId}' was not found in the education service."));
            }

            choice = new StudentAdmissionChoice(Guid.NewGuid(), command.StudentId, profile.SchoolId);
            context.StudentAdmissionChoices.Add(choice);
        }

        if (command.NmtScores is { Count: > 0 })
        {
            choice.SetNmtScores(command.NmtScores, clock.UtcNow);
        }

        if (command.ChosenFourthSubject is { } subject)
        {
            choice.SetFourthSubject(subject, clock.UtcNow);
        }

        if (!string.IsNullOrWhiteSpace(command.DesiredDirectionCode))
        {
            choice.SetDesiredDirection(command.DesiredDirectionCode, clock.UtcNow);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
