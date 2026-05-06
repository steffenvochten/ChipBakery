using ChipBakery.Shared;
using FluentValidation;

namespace Warehouse.Application.Validators;

public class CreateWarehouseItemValidator : AbstractValidator<CreateWarehouseItemRequest>
{
    public CreateWarehouseItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
