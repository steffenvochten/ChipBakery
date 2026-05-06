namespace Loyalty.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public class LoyaltyMemberNotFoundException(Guid id) : DomainException($"Loyalty member with ID {id} was not found.");
public class InsufficientPointsException(Guid id, int required, int available) 
    : DomainException($"Loyalty member with ID {id} has insufficient points. Required: {required}, Available: {available}.");
