using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NewRuleTypesValidator : ReactiveValidator<NewRuleTypesModel>
    {
        public NewRuleTypesValidator()
        {
            RuleFor(x => x.CardNumber).CreditCard().WithMessage("Card number is not valid.");

            RuleFor(x => x.Score).ExclusiveBetween(0m, 100m).WithMessage("Score must be between 0 and 100 (exclusive).");

            RuleFor(x => x.MonthlyRate).GreaterThan(0m).WithMessage("Monthly rate must be greater than zero.");

            RuleFor(x => x.MaxDeposit).LessThan(1000000m).WithMessage("Max deposit must be less than 1,000,000.");

            RuleFor(x => x.Status).NotEqual("deleted").WithMessage("Status must not be 'deleted'.");

            RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email).WithMessage("Alternate email must differ from primary email.");

            RuleFor(x => x.Website).Url().WithMessage("Website must be a valid URL.");

            RuleFor(x => x.Nickname).Empty().WithMessage("Nickname must be empty.");
        }
    }
}
