using FluentValidation;
using ChipBakery.Shared;

namespace Supplier.Application.Validators;

public class CreateIngredientSupplyValidator : AbstractValidator<CreateIngredientSupplyRequest>
{
    public CreateIngredientSupplyValidator()
    {
        RuleFor(x => x.IngredientName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.ScheduledDate).NotEmpty();
    }
}

public class UpdateIngredientSupplyValidator : AbstractValidator<UpdateIngredientSupplyRequest>
{
    public UpdateIngredientSupplyValidator()
    {
        RuleFor(x => x.IngredientName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.ScheduledDate).NotEmpty();
    }
}
