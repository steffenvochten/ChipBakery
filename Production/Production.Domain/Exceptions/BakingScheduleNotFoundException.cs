namespace Production.Domain.Exceptions;

public class BakingScheduleNotFoundException(Guid id) 
    : DomainException($"Baking schedule with ID {id} was not found.");
