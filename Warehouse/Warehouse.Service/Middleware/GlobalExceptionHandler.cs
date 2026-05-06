using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Warehouse.Domain.Exceptions;

namespace Warehouse.Service.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, type) = exception switch
        {
            WarehouseItemNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Warehouse Item Not Found",
                ex.Message,
                "https://chipbakery.dev/errors/not-found"),

            InsufficientStockException ex => (
                StatusCodes.Status409Conflict,
                "Insufficient Stock",
                ex.Message,
                "https://chipbakery.dev/errors/insufficient-stock"),

            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                "https://chipbakery.dev/errors/validation"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                "https://chipbakery.dev/errors/internal")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
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

        return true;
    }
}
