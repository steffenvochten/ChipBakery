using ChipBakery.Shared;
using FluentValidation;

namespace Supplier.Application.Validators;

public class RestockRequestValidator : AbstractValidator<RestockRequest>
{
    public RestockRequestValidator()
    {
        RuleFor(x => x.IngredientName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty();
    }
}
