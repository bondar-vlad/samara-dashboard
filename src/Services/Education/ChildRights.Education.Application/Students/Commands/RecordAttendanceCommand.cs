using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.Results;
using ChildRights.Contracts.Education;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Application.Common;
using ChildRights.Education.Domain.Entities;
using ChildRights.Education.Domain.Enums;
using ChildRights.Education.Domain.Policies;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Education.Application.Students.Commands;

/// <summary>Records an attendance entry and evaluates the attendance red-flag policy.</summary>
public sealed record RecordAttendanceCommand(
    Guid StudentId,
    DateOnly Date,
    AttendanceStatus Status,
    string? Subject) : ICommand<RecordAttendanceResponse>;

public sealed record RecordAttendanceResponse(int UnexcusedAbsences, string Severity, bool NotificationTriggered);

public sealed class RecordAttendanceCommandValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Date)
            .Must(date => date != default)
            .WithMessage("A valid attendance date is required.");
    }
}

internal sealed class RecordAttendanceCommandHandler(
    IEducationDbContext context,
    IEventPublisher eventPublisher,
    IClock clock) : ICommandHandler<RecordAttendanceCommand, RecordAttendanceResponse>
{
    public async Task<Result<RecordAttendanceResponse>> Handle(
        RecordAttendanceCommand command,
        CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == command.StudentId, cancellationToken);

        if (student is null)
        {
            return Result.Failure<RecordAttendanceResponse>(
                Error.NotFound($"Student '{command.StudentId}' was not found."));
        }

        context.AttendanceRecords.Add(new AttendanceRecord(
            Guid.NewGuid(), student.Id, command.Date, command.Status, command.Subject));

        await context.SaveChangesAsync(cancellationToken);

        var unexcused = await context.AttendanceRecords
            .CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Unexcused, cancellationToken);

        var severity = AttendancePolicy.Evaluate(unexcused);
        var notificationTriggered = AttendancePolicy.RequiresNotification(unexcused);

        // When the threshold is crossed, broadcast an integration event so the Analysis
        // service can raise a red flag and the Notifications service can escalate.
        if (notificationTriggered)
        {
            await eventPublisher.PublishAsync(
                new AttendanceThresholdReachedIntegrationEvent
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    ClassId = student.ClassId,
                    SchoolId = student.SchoolId,
                    UnexcusedAbsences = unexcused,
                    Period = AcademicCalendar.CurrentPeriod(clock.UtcNow)
                },
                cancellationToken);
        }

        return new RecordAttendanceResponse(unexcused, severity.ToString(), notificationTriggered);
    }
}
