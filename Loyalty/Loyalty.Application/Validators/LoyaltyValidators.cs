using FluentValidation;
using Loyalty.Application.DTOs;

namespace Loyalty.Application.Validators;

public class CreateLoyaltyMemberValidator : AbstractValidator<CreateLoyaltyMemberRequest>
{
    public CreateLoyaltyMemberValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class AddPointsValidator : AbstractValidator<AddPointsRequest>
{
    public AddPointsValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}

public class DeductPointsValidator : AbstractValidator<DeductPointsRequest>
{
    public DeductPointsValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}
