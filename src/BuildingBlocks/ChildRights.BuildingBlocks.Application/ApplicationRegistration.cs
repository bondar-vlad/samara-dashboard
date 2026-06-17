using System.Reflection;
using ChildRights.BuildingBlocks.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.BuildingBlocks.Application;

/// <summary>
/// Registers the dispatcher, auto-discovers every command/query handler and every
/// FluentValidation validator in the supplied assemblies, so each service only calls
/// <c>AddApplication(typeof(...).Assembly)</c>.
/// </summary>
public static class ApplicationRegistration
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly
                .GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false });

            foreach (var implementation in handlerTypes)
            {
                var closedInterfaces = implementation
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

                foreach (var serviceInterface in closedInterfaces)
                {
                    services.AddScoped(serviceInterface, implementation);
                }
            }

            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        }

        return services;
    }
}
