using FluentValidation;
using Supplier.Application.DTOs;

namespace Supplier.Application.Validators;

public class DispatchTransportRequestValidator : AbstractValidator<DispatchTransportRequest>
{
    public DispatchTransportRequestValidator()
    {
        RuleFor(x => x.IngredientName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
    }
}
