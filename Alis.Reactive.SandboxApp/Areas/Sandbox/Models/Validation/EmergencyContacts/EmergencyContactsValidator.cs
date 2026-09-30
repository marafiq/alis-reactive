using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EmergencyContactValidator : ReactiveValidator<EmergencyContact>
    {
        public EmergencyContactValidator()
        {
            ClientRule(x => x.Phone).Required("Phone is required.");
        }
    }

    public class EmergencyContactsValidator : ReactiveValidator<EmergencyContactsModel>
    {
        public EmergencyContactsValidator()
        {
            ClientRule(x => x.ResidentName).Required("Resident name is required.");

            WhenField(x => x.HasContacts, () =>
                ClientRuleEach(x => x.Contacts).SetValidator(new EmergencyContactValidator()));
        }
    }
}
