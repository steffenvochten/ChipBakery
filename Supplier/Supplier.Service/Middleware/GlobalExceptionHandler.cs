using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Supplier.Domain.Exceptions;

namespace Supplier.Service.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail, type) = exception switch
        {
            IngredientSupplyNotFoundException ex => (404, "Not Found", ex.Message, "errors/not-found"),
            ValidationException ex => (400, "Validation Failed", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)), "errors/validation"),
            DomainException ex => (400, "Domain Error", ex.Message, "errors/domain-error"),
            _ => (500, "Internal Server Error", "An unexpected error occurred.", "errors/internal-error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
