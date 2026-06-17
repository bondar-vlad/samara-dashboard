using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChildRights.Analysis.Infrastructure.Scheduling;

/// <summary>
/// Proactive trigger: periodically re-analyses pupils on a schedule. Disabled by
/// default (Ai:ScheduleMinutes = 0); enable it to demonstrate the "by schedule" mode
/// (e.g. a nightly absence sweep).
/// </summary>
internal sealed class ScheduledAnalysisService(
    IServiceScopeFactory scopeFactory,
    IOptions<AiOptions> options,
    ILogger<ScheduledAnalysisService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = options.Value.ScheduleMinutes;
        if (minutes <= 0)
        {
            logger.LogInformation("Scheduled analysis is disabled (Ai:ScheduleMinutes = 0).");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));

        try
        {
            do
            {
                try
                {
                    await RunSweepAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Scheduled analysis sweep failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private async Task RunSweepAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var educationClient = scope.ServiceProvider.GetRequiredService<IEducationDataClient>();
        var engine = scope.ServiceProvider.GetRequiredService<IAnalysisEngine>();

        var students = await educationClient.GetStudentsAsync(options.Value.ScheduledSchoolId, cancellationToken);
        foreach (var student in students)
        {
            await engine.AnalyzeStudentAsync(student.Id, AnalysisTrigger.Scheduled, cancellationToken: cancellationToken);
        }

        logger.LogInformation("Scheduled sweep analysed {Count} pupils.", students.Count);
    }
}
