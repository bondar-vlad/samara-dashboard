using ChildRights.BuildingBlocks.Application.Abstractions;

namespace ChildRights.BuildingBlocks.Infrastructure.Hosting;

/// <summary>Default <see cref="IClock"/> backed by the system UTC clock.</summary>
internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
