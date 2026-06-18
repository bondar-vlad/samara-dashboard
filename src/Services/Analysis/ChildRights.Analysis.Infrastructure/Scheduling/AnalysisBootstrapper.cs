using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChildRights.Analysis.Infrastructure.Scheduling;

/// <summary>
/// One-time startup sweep that analyses every pupil so the platform <b>automatically</b> detects
/// risks (red flags) and produces recommendations as soon as the data is available — the core
/// "detect risks early and trigger targeted support before the situation becomes critical" promise
/// — without waiting for an incoming data event or a manual run.
///
/// It runs once: if any analysis run already exists it does nothing, so restarts are cheap and it
/// never duplicates work. The deterministic rule engine means no external AI dependency is needed.
/// </summary>
internal sealed class AnalysisBootstrapper(
    IServiceScopeFactory scopeFactory,
    IOptions<AiOptions> options,
    ILogger<AnalysisBootstrapper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.BootstrapAnalysisOnStartup)
        {
            logger.LogInformation("Startup analysis bootstrap is disabled (Ai:BootstrapAnalysisOnStartup = false).");
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<IAnalysisDbContext>();

            // Run once: if the platform has already analysed anyone, the data is already populated.
            if (await context.AnalysisRuns.AnyAsync(stoppingToken))
            {
                logger.LogInformation("Startup analysis bootstrap skipped — analysis runs already exist.");
                return;
            }

            var educationClient = scope.ServiceProvider.GetRequiredService<IEducationDataClient>();
            var engine = scope.ServiceProvider.GetRequiredService<IAnalysisEngine>();

            IReadOnlyList<EducationStudentRef> students;
            try
            {
                students = await educationClient.GetStudentsAsync(null, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Startup analysis bootstrap skipped — the Education service is unreachable. " +
                    "It will retry on the next startup.");
                return;
            }

            // Analyse school by school (lowest id first) so the demo anchor school lights up quickly.
            var ordered = students
                .OrderBy(s => s.SchoolId.ToString(), StringComparer.Ordinal)
                .ThenBy(s => s.FullName)
                .ToList();

            logger.LogInformation("Startup analysis bootstrap started for {Count} pupils.", ordered.Count);

            var analysed = 0;
            var failed = 0;
            foreach (var student in ordered)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await engine.AnalyzeStudentAsync(
                        student.Id, AnalysisTrigger.Scheduled, cancellationToken: stoppingToken);
                    analysed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    logger.LogWarning(ex, "Startup analysis failed for pupil {StudentId}.", student.Id);
                }
            }

            logger.LogInformation(
                "Startup analysis bootstrap completed: analysed {Analysed} pupils ({Failed} failed).",
                analysed, failed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Startup analysis bootstrap failed.");
        }
    }
}
