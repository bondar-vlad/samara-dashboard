using System.Reflection;
using ChildRights.BuildingBlocks.Domain.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ChildRights.BuildingBlocks.Application.Messaging;

/// <summary>
/// Resolves the correct handler for a command/query at runtime from the DI container,
/// running any registered FluentValidation validators first (validation pipeline).
/// </summary>
internal sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = provider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.Handle))!;
        return await (Task<Result>)method.Invoke(handler, [command, cancellationToken])!;
    }

    public async Task<Result<TResponse>> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<TResponse>(validationError);
        }

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        return await Invoke<TResponse>(handlerType, command, cancellationToken);
    }

    public async Task<Result<TResponse>> Query<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        return await Invoke<TResponse>(handlerType, query, cancellationToken);
    }

    private Task<Result<TResponse>> Invoke<TResponse>(
        Type handlerType,
        object request,
        CancellationToken cancellationToken)
    {
        var handler = provider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task<Result<TResponse>>)method.Invoke(handler, [request, cancellationToken])!;
    }

    private async Task<Error?> ValidateAsync(object request, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        var validators = provider.GetServices(validatorType).OfType<IValidator>().ToList();
        if (validators.Count == 0)
        {
            return null;
        }

        var context = new ValidationContext<object>(request);
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                return Error.Validation(string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
            }
        }

        return null;
    }
}
