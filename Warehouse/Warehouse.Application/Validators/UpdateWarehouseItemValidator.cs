using FluentValidation;
using Warehouse.Application.DTOs;

namespace Warehouse.Application.Validators;

public class UpdateWarehouseItemValidator : AbstractValidator<UpdateWarehouseItemRequest>
{
    public UpdateWarehouseItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
