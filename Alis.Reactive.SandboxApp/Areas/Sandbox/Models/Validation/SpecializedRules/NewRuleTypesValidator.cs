using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NewRuleTypesValidator : ReactiveValidator<NewRuleTypesModel>
    {
        public NewRuleTypesValidator()
        {
            // creditCard
            ClientRule(x => x.CardNumber)
                .CreditCard("Card number is not valid.");

            // exclusiveRange — score must be strictly between 0 and 100
            ClientRule(x => x.Score)
                .ExclusiveRange(0m, 100m, "Score must be between 0 and 100 (exclusive).");

            // gt — monthly rate must be greater than zero (implies required)
            ClientRule(x => x.MonthlyRate)
                .GreaterThan(0m, "Monthly rate must be greater than zero.");

            // lt — max deposit must be less than 1,000,000
            ClientRule(x => x.MaxDeposit)
                .LessThan(1000000m, "Max deposit must be less than 1,000,000.");

            // notEqual fixed value — status must not be "deleted"
            ClientRule(x => x.Status)
                .NotEqual("deleted", "Status must not be 'deleted'.");

            // notEqualTo cross-property — alternate email must differ from primary
            ClientRule(x => x.AlternateEmail)
                .NotEqualTo(x => x.Email, "Alternate email must differ from primary email.");

            // url
            ClientRule(x => x.Website)
                .Url("Website must be a valid URL.");

            // empty — nickname must be empty
            ClientRule(x => x.Nickname)
                .Empty("Nickname must be empty.");
        }
    }
}
