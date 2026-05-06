using FluentValidation;
using Order.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Order.Service.Middleware;

/// <summary>
/// Translates domain and validation exceptions into RFC 7807 ProblemDetails responses.
/// Registered via app.UseExceptionHandler() + services.AddProblemDetails().
/// Works natively with Aspire's OpenTelemetry — exceptions are still captured in traces.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, type) = exception switch
        {
            OrderNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Order Not Found",
                ex.Message,
                "https://chipbakery.dev/errors/not-found"),

            // Thrown when cancelling an order that is not in Placed status
            InvalidOperationException ex => (
                StatusCodes.Status409Conflict,
                "Operation Not Allowed",
                ex.Message,
                "https://chipbakery.dev/errors/conflict"),

            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                "https://chipbakery.dev/errors/validation"),

            // Catch-all — log at Error level, return 500
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                "https://chipbakery.dev/errors/internal")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Marks the exception as handled — no further processing
    }
}
