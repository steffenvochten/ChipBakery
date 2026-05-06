using FluentValidation;
using Warehouse.Application.DTOs;

namespace Warehouse.Application.Validators;

public class DeductStockValidator : AbstractValidator<DeductStockRequest>
{
    public DeductStockValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
