using ChildRights.BuildingBlocks.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Wires the integration-event bus. When <c>MessageBus:Host</c> is configured the
/// platform uses RabbitMQ; otherwise it falls back to MassTransit's in-memory transport
/// so a single service still runs locally without a broker.
/// </summary>
public static class MessagingExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var host = configuration["MessageBus:Host"];

        services.AddMassTransit(registration =>
        {
            registration.SetKebabCaseEndpointNameFormatter();
            configureConsumers?.Invoke(registration);

            if (string.IsNullOrWhiteSpace(host))
            {
                registration.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                registration.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, configuration["MessageBus:VirtualHost"] ?? "/", h =>
                    {
                        h.Username(configuration["MessageBus:Username"] ?? "guest");
                        h.Password(configuration["MessageBus:Password"] ?? "guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }
}
