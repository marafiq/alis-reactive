using FluentValidation;
using Alis.Reactive.FluentValidator.Validators;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.SpecializedRules
{
    public class NewRuleTypesValidator : AbstractValidator<NewRuleTypesModel>
    {
        public NewRuleTypesValidator()
        {
            // creditCard
            RuleFor(x => x.CardNumber).CreditCard()
                .WithMessage("Card number is not valid.");

            // exclusiveRange — score must be strictly between 0 and 100
            RuleFor(x => x.Score).IsExclusiveBetween(0m, 100m)
                .WithMessage("Score must be between 0 and 100 (exclusive).");

            // gt — monthly rate must be greater than zero (implies required)
            RuleFor(x => x.MonthlyRate).GreaterThan(0m)
                .WithMessage("Monthly rate must be greater than zero.");

            // lt — max deposit must be less than 1,000,000
            RuleFor(x => x.MaxDeposit).LessThan(1000000m)
                .WithMessage("Max deposit must be less than 1,000,000.");

            // notEqual fixed value — status must not be "deleted"
            RuleFor(x => x.Status).NotEqual("deleted")
                .WithMessage("Status must not be 'deleted'.");

            // notEqualTo cross-property — alternate email must differ from primary
            RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email)
                .WithMessage("Alternate email must differ from primary email.");

            // url
            RuleFor(x => x.Website).Matches(@"^https?:\/\/.+")
                .WithMessage("Website must be a valid URL.");

            // empty — nickname must be empty
            RuleFor(x => x.Nickname).IsEmpty()
                .WithMessage("Nickname must be empty.");
        }
    }
}
