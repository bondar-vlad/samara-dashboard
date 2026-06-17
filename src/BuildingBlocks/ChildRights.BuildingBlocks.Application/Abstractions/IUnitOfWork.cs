namespace ChildRights.BuildingBlocks.Application.Abstractions;

/// <summary>Abstracts the transactional boundary so handlers can persist a unit of work.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Supplies the current UTC time; injectable so logic stays testable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
