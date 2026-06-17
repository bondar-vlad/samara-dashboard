using ChildRights.BuildingBlocks.Domain.Results;

namespace ChildRights.BuildingBlocks.Application.Messaging;

/// <summary>A command that mutates state and returns no value.</summary>
public interface ICommand
{
}

/// <summary>A command that mutates state and returns a value.</summary>
public interface ICommand<TResponse>
{
}

/// <summary>A read-only request that returns data and never mutates state.</summary>
public interface IQuery<TResponse>
{
}

/// <summary>Handles a <see cref="ICommand"/>.</summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Handles a <see cref="ICommand{TResponse}"/>.</summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Handles an <see cref="IQuery{TResponse}"/>.</summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
