namespace Order.Domain.Exceptions;

/// <summary>
/// Abstract base for all Order domain exceptions.
/// Caught by GlobalExceptionHandler and mapped to ProblemDetails responses.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
