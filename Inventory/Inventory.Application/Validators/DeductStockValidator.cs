using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators;

public class DeductStockValidator : AbstractValidator<DeductStockRequest>
{
    public DeductStockValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity to deduct must be greater than zero.");
    }
}
