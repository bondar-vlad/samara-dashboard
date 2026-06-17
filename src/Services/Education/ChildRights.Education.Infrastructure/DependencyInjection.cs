using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Infrastructure.Persistence;
using ChildRights.Education.Application.Abstractions;
using ChildRights.Education.Infrastructure.Messaging;
using ChildRights.Education.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.Education.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEducationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPostgresDbContext<EducationDbContext>(configuration);

        services.AddScoped<IEducationDbContext>(sp => sp.GetRequiredService<EducationDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EducationDbContext>());
        services.AddScoped<IDataSeeder, EducationDataSeeder>();

        return services;
    }

    /// <summary>Registers the integration-event consumers with the MassTransit bus.</summary>
    public static void AddEducationConsumers(IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<StudentProfileRecommendedConsumer>();
    }
}
