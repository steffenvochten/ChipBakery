namespace Inventory.Domain.Exceptions;

/// <summary>
/// Base class for all domain-layer exceptions in Inventory.Service.
/// The API exception handler catches subtypes of this and maps them to
/// appropriate HTTP ProblemDetails responses.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
