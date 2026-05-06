using FluentValidation;
using Production.Application.DTOs;

namespace Production.Application.Validators;

/// <summary>
/// Validates the input for scheduling a new baking job.
/// </summary>
public class ScheduleBakingJobRequestValidator : AbstractValidator<ScheduleBakingJobRequest>
{
    public ScheduleBakingJobRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
