using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Commands;

/// <summary>Records a subject grade (Ukrainian 1–12 scale) for a pupil, optionally for a topic.</summary>
public sealed record RecordGradeCommand(
    Guid StudentId,
    string Subject,
    int Value,
    string Term,
    string? Topic = null) : ICommand<Guid>;

public sealed class RecordGradeCommandValidator : AbstractValidator<RecordGradeCommand>
{
    public RecordGradeCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Value).InclusiveBetween(1, 12);
        RuleFor(x => x.Term).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Topic).MaximumLength(160);
    }
}

internal sealed class RecordGradeCommandHandler(IEducationDbContext context)
    : ICommandHandler<RecordGradeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RecordGradeCommand command, CancellationToken cancellationToken)
    {
        var studentExists = await context.Students
            .AnyAsync(s => s.Id == command.StudentId, cancellationToken);

        if (!studentExists)
        {
            return Result.Failure<Guid>(Error.NotFound($"Student '{command.StudentId}' was not found."));
        }

        var grade = new Grade(
            Guid.NewGuid(),
            command.StudentId,
            command.Subject,
            command.Value,
            command.Term,
            DateOnly.FromDateTime(DateTime.UtcNow),
            string.IsNullOrWhiteSpace(command.Topic) ? null : command.Topic.Trim());

        context.Grades.Add(grade);
        await context.SaveChangesAsync(cancellationToken);

        return grade.Id;
    }
}
