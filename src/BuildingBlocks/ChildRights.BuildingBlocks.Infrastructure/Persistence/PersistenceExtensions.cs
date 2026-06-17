using ChildRights.BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChildRights.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Shared PostgreSQL / EF Core registration and startup initialization helpers.</summary>
public static class PersistenceExtensions
{
    public static IServiceCollection AddPostgresDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Database")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' is not configured.");

        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(TContext).Assembly.FullName)));

        return services;
    }

    /// <summary>
    /// Ensures the schema exists and runs every registered <see cref="IDataSeeder"/>.
    /// For the demo this uses <c>EnsureCreated</c>; production should switch to migrations.
    /// Failures are logged but do not crash the host, so the API (and Swagger) still start
    /// — useful when the database is starting up alongside the service.
    /// </summary>
    public static async Task InitializeDatabaseAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.EnsureCreatedAsync();

            foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
            {
                await seeder.SeedAsync();
            }

            logger.LogInformation("Database initialization for {Context} completed.", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Database initialization for {Context} failed. The service will start, but database-backed " +
                "endpoints will not work until the connection succeeds.", typeof(TContext).Name);
        }
    }
}
