using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EmergencyContactValidator : ReactiveValidator<EmergencyContact>
    {
        /// <summary>The community's front desk, which families sometimes give instead of their own number.</summary>
        public const string FrontDeskPhone = "555-0199";

        public EmergencyContactValidator()
        {
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required.");

            // The server checks the number against the community's directory; the browser cannot.
            RuleFor(x => x.Phone)
                .Must(phone => phone != FrontDeskPhone)
                .WithMessage($"{FrontDeskPhone} is the front desk. Enter the contact's own number.");
        }
    }

    public class EmergencyContactsValidator : ReactiveValidator<EmergencyContactsModel>
    {
        public EmergencyContactsValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");

            When(x => x.HasContacts, () =>
                RuleForEach(x => x.Contacts).SetValidator(new EmergencyContactValidator()));
        }
    }
}
