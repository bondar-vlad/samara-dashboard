using System.Reflection;
using ChildRights.BuildingBlocks.Application;
using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Application.Engine;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.Analysis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalysisApplication(this IServiceCollection services)
    {
        services.AddApplication(Assembly.GetExecutingAssembly());
        services.AddScoped<IAnalysisEngine, AnalysisEngine>();
        return services;
    }
}
