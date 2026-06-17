using System.Net.Http.Headers;
using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Infrastructure.Ai;
using ChildRights.Analysis.Infrastructure.Configuration;
using ChildRights.Analysis.Infrastructure.Education;
using ChildRights.Analysis.Infrastructure.Messaging;
using ChildRights.Analysis.Infrastructure.Persistence;
using ChildRights.Analysis.Infrastructure.Scheduling;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.Analysis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalysisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddPostgresDbContext<AnalysisDbContext>(configuration);
        services.AddScoped<IAnalysisDbContext>(sp => sp.GetRequiredService<AnalysisDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AnalysisDbContext>());

        // Resilient typed client to the Education microservice.
        var educationBaseUrl = configuration["Services:Education"] ?? "http://localhost:5101";
        services.AddHttpClient<IEducationDataClient, EducationDataClient>(client =>
                client.BaseAddress = new Uri(educationBaseUrl))
            .AddStandardResilienceHandler();

        // The deterministic engine is always available.
        services.AddSingleton<IAiAnalysisProvider, RuleBasedAiAnalysisProvider>();

        // The LLM provider is registered only when an API key is configured.
        var aiOptions = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
        if (aiOptions.OpenAi.Enabled)
        {
            services.AddHttpClient(OpenAiAnalysisProvider.HttpClientName, client =>
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", aiOptions.OpenAi.ApiKey))
                .AddStandardResilienceHandler();

            services.AddSingleton<IAiAnalysisProvider, OpenAiAnalysisProvider>();
        }

        services.AddSingleton<IAiAnalysisProviderFactory, AiAnalysisProviderFactory>();

        services.AddHostedService<ScheduledAnalysisService>();

        return services;
    }

    /// <summary>Registers the integration-event consumers with the MassTransit bus.</summary>
    public static void AddAnalysisConsumers(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<AttendanceThresholdReachedConsumer>();
        configurator.AddConsumer<MedicalConcernReportedConsumer>();
        configurator.AddConsumer<BullyingReportFiledConsumer>();
    }
}
