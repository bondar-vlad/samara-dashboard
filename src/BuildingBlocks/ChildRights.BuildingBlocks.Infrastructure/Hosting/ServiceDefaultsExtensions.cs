using ChildRights.BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

namespace ChildRights.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// One-line, opinionated host configuration shared by every microservice:
/// structured logging, ProblemDetails errors, health checks and Swagger/OpenAPI.
/// </summary>
public static class ServiceDefaultsExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder, string apiTitle)
    {
        builder.Services.AddSerilog((_, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        builder.Services.AddSingleton<IClock, SystemClock>();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddHealthChecks();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = apiTitle,
                Version = "v1",
                Description = "Part of the Child Rights Monitoring Dashboard platform."
            });
        });

        return builder;
    }

    /// <summary>Registers MVC controllers with enums serialized as readable strings.</summary>
    public static IMvcBuilder AddApiControllers(this IServiceCollection services) =>
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.DocumentTitle = "Child Rights Monitoring API";
        });

        app.MapHealthChecks("/health");

        return app;
    }
}
