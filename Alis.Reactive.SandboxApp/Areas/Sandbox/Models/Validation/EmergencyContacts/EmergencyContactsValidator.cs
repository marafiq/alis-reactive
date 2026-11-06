namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EmergencyContactValidator : ReactiveValidator<EmergencyContact>
    {
        public const string FrontDeskPhone = "555-0199";

        public EmergencyContactValidator()
        {
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required.");

            RuleFor(x => x.Phone)
                .Must(phone => phone != FrontDeskPhone)
                .WithMessage($"{FrontDeskPhone} is the front desk. Enter the contact's own number.");
        }
    }

    public class EmergencyContactsSaveValidator : ReactiveValidator<EmergencyContactsSave>
    {
        public EmergencyContactsSaveValidator()
        {
            When(x => x.HasContacts, () =>
                RuleForEach(x => x.Contacts).SetValidator(new EmergencyContactValidator()));
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
