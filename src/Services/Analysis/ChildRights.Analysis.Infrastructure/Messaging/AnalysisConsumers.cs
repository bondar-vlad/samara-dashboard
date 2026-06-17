using ChildRights.BuildingBlocks.Application.Messaging;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Contracts.Education;
using ChildRights.Contracts.JuvenilePolice;
using ChildRights.Contracts.Medical;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Flags.Commands;
using MassTransit;

namespace ChildRights.Analysis.Infrastructure.Messaging;

/// <summary>Reactive trigger: a pupil crossed the attendance threshold → run a full analysis.</summary>
public sealed class AttendanceThresholdReachedConsumer(IAnalysisEngine engine)
    : IConsumer<AttendanceThresholdReachedIntegrationEvent>
{
    public Task Consume(ConsumeContext<AttendanceThresholdReachedIntegrationEvent> context) =>
        engine.AnalyzeStudentAsync(
            context.Message.StudentId,
            AnalysisTrigger.Event,
            cancellationToken: context.CancellationToken);
}

/// <summary>Reactive trigger: recurring medical concern → raise a medical red flag.</summary>
public sealed class MedicalConcernReportedConsumer(IDispatcher dispatcher)
    : IConsumer<MedicalConcernReportedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MedicalConcernReportedIntegrationEvent> context)
    {
        var message = context.Message;

        await dispatcher.Send(
            new RaiseRedFlagCommand(
                "MED-RECURRING",
                AnalysisScope.Student,
                message.StudentId,
                message.StudentName,
                FlagSeverity.Orange,
                "Часті звернення за медичною допомогою",
                $"{message.OccurrencesInPeriod} звернень за категорією «{message.ConditionCategory}».",
                Agency.Medical,
                [AudienceRole.Parent, AudienceRole.MedicalService, AudienceRole.ClassTeacher],
                ["Рекомендувати консультацію профільного лікаря", message.RecommendedReferral]),
            context.CancellationToken);
    }
}

/// <summary>Reactive trigger: a bullying report for a class → raise a class-level red flag.</summary>
public sealed class BullyingReportFiledConsumer(IDispatcher dispatcher)
    : IConsumer<BullyingReportFiledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BullyingReportFiledIntegrationEvent> context)
    {
        var message = context.Message;

        await dispatcher.Send(
            new RaiseRedFlagCommand(
                "JUV-BULLYING",
                AnalysisScope.Class,
                message.ClassId,
                "Клас",
                message.Severity,
                "Звернення щодо булінгу у класі",
                message.Summary,
                Agency.JuvenilePolice,
                [
                    AudienceRole.ClassTeacher,
                    AudienceRole.SchoolAdministration,
                    AudienceRole.JuvenilePolice,
                    AudienceRole.EducationSafetyOfficer
                ],
                ["Провести профілактичну роботу з класом", "Залучити ювенальну поліцію та шкільного психолога"]),
            context.CancellationToken);
    }
}
