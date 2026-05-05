using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators;

public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemRequest>
{
    public UpdateInventoryItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}
