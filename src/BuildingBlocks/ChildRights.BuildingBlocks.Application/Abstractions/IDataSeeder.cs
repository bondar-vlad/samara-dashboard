namespace ChildRights.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Implemented per service to seed demo/reference data on startup. All registered
/// seeders are executed by the host after the database schema is ensured.
/// </summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
