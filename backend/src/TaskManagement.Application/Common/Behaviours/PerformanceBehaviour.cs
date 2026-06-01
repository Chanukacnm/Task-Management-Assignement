using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Application.Common.Behaviours;

/// <summary>
/// Times each request and logs a warning when a handler runs longer than the
/// configured threshold, surfacing slow operations.
/// </summary>
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long LongRunningThresholdMs = 500;

    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        var response = await next();

        timer.Stop();

        if (timer.ElapsedMilliseconds > LongRunningThresholdMs)
        {
            _logger.LogWarning(
                "Long-running request: {RequestName} took {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}
