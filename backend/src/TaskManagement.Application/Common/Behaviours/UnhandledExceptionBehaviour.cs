using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Application.Common.Behaviours;

/// <summary>
/// Logs unexpected exceptions thrown by handlers. Expected, already-handled
/// application exceptions (validation, not-found) are left to propagate quietly
/// to the API's exception middleware.
/// </summary>
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception) when (exception is not ValidationException and not NotFoundException)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for request {RequestName}",
                typeof(TRequest).Name);

            throw;
        }
    }
}
