using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and converts them into consistent
/// <see cref="ProblemDetails"/> responses (RFC 7807).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            NotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("{Title} ({Status}) for {Method} {Path}: {Message}",
                title, statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        // Only validation errors echo their (generic) message; other errors return a
        // generic detail so internal context (entity names, ids) is not leaked.
        var detail = exception switch
        {
            ValidationException => exception.Message,
            _ when statusCode == StatusCodes.Status500InternalServerError
                => "An unexpected error occurred. Please try again later.",
            _ => title
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path,
            Detail = detail
        };

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
