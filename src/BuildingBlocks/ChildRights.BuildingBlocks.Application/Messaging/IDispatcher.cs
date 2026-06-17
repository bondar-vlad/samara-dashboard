using ChildRights.BuildingBlocks.Domain.Results;

namespace ChildRights.BuildingBlocks.Application.Messaging;

/// <summary>
/// Routes commands and queries to their registered handlers. A tiny in-house
/// mediator so the platform avoids a third-party (and now commercially licensed)
/// dependency while keeping handlers decoupled from controllers.
/// </summary>
public interface IDispatcher
{
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);

    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    Task<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
