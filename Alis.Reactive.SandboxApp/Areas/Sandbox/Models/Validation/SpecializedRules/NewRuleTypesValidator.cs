using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NewRuleTypesValidator : ReactiveValidator<NewRuleTypesModel>
    {
        public NewRuleTypesValidator()
        {
            ClientRule(x => x.CardNumber)
                .CreditCard("Card number is not valid.");

            ClientRule(x => x.Score)
                .ExclusiveRange(0m, 100m, "Score must be between 0 and 100 (exclusive).");

            ClientRule(x => x.MonthlyRate)
                .GreaterThan(0m, "Monthly rate must be greater than zero.");

            ClientRule(x => x.MaxDeposit)
                .LessThan(1000000m, "Max deposit must be less than 1,000,000.");

            ClientRule(x => x.Status)
                .NotEqual("deleted", "Status must not be 'deleted'.");

            ClientRule(x => x.AlternateEmail)
                .NotEqualTo(x => x.Email, "Alternate email must differ from primary email.");

            ClientRule(x => x.Website)
                .Url("Website must be a valid URL.");

            ClientRule(x => x.Nickname)
                .Empty("Nickname must be empty.");
        }
    }
}
