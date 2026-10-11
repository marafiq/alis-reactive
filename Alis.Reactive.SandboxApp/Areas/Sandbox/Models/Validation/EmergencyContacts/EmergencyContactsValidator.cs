using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EmergencyContactValidator : ReactiveValidator<EmergencyContact>
    {
        public EmergencyContactValidator()
        {
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone is required.");
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
