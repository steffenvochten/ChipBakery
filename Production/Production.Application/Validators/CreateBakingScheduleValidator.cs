using FluentValidation;
using Production.Application.DTOs;

namespace Production.Application.Validators;

public class CreateBakingScheduleValidator : AbstractValidator<CreateBakingScheduleRequest>
{
    public CreateBakingScheduleValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ScheduledTime).NotEmpty();
    }
}
