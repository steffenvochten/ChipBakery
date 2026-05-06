using FluentValidation;
using Order.Application.DTOs;

namespace Order.Application.Validators;

/// <summary>
/// Validates the input for placing a new order.
/// Called at the top of <see cref="Order.Application.Services.OrderService.PlaceOrderAsync"/>.
/// </summary>
public class PlaceOrderValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");
    }
}
