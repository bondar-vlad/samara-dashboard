using System.Reflection;
using ChildRights.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.Education.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEducationApplication(this IServiceCollection services)
        => services.AddApplication(Assembly.GetExecutingAssembly());
}
